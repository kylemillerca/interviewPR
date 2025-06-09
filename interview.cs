using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace OrderSystem
{
    public class OrderService
    {
        private string cs = "Server=localhost;Database=OrderDB;Integrated Security=true;";
        
        public string ProcessOrderAndUpdateEverything(int oid, string e, string fn, string ln, List<int> pids, List<int> qtys, bool rush, string cc, string addr)
        {
            var sb = new StringBuilder();
            var totalPrice = 0.0;
            var errors = new List<string>();
            
            try
            {
                if (oid <= 0 || string.IsNullOrEmpty(e) || pids == null || qtys == null || pids.Count != qtys.Count)
                {
                    return "Invalid input data";
                }
                
                var conn = new SqlConnection(cs);
                conn.Open();
                
                // Check if order already exists
                var checkCmd = new SqlCommand($"SELECT COUNT(*) FROM Orders WHERE OrderId = {oid}", conn);
                var exists = (int)checkCmd.ExecuteScalar();
                if (exists > 0)
                {
                    conn.Close();
                    return "Order already exists";
                }
                
                // Process each product
                for (int i = 0; i < pids.Count; i++)
                {
                    var pid = pids[i];
                    var qty = qtys[i];
                    
                    // Get product details 
                    var productCmd = new SqlCommand($"SELECT ProductName, Price, StockQuantity FROM Products WHERE ProductId = {pid}", conn);
                    var reader = productCmd.ExecuteReader();
                    
                    if (reader.Read())
                    {
                        var pname = reader["ProductName"].ToString();
                        var price = Convert.ToDouble(reader["Price"]);
                        var stock = Convert.ToInt32(reader["StockQuantity"]);
                        reader.Close();
                        
                        if (stock < qty)
                        {
                            errors.Add($"Insufficient stock for {pname}");
                            continue;
                        }
                        
                        // Calculate pricing
                        var itemTotal = price * qty;
                        if (qty > 10)
                        {
                            if (qty > 50)
                            {
                                if (qty > 100)
                                {
                                    itemTotal = itemTotal * 0.8; // 20% discount
                                }
                                else
                                {
                                    itemTotal = itemTotal * 0.85; // 15% discount
                                }
                            }
                            else
                            {
                                itemTotal = itemTotal * 0.9; // 10% discount
                            }
                        }
                        
                        if (rush)
                        {
                            itemTotal = itemTotal * 1.25; // Rush fee
                        }
                        
                        totalPrice += itemTotal;
                        sb.AppendLine($"{pname} x {qty} = ${itemTotal:F2}");
                        
                        // Update inventory 
                        var updateCmd = new SqlCommand($"UPDATE Products SET StockQuantity = StockQuantity - {qty} WHERE ProductId = {pid}", conn);
                        updateCmd.ExecuteNonQuery();
                        
                        // Log to database 
                        var logCmd = new SqlCommand($"INSERT INTO Logs (Message, Timestamp) VALUES ('Updated inventory for product {pid}', '{DateTime.Now}')", conn);
                        logCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        reader.Close();
                        errors.Add($"Product {pid} not found");
                    }
                }
                
                if (errors.Count > 0)
                {
                    conn.Close();
                    return "Errors: " + string.Join(", ", errors);
                }
                
                // Apply additional discounts based on customer type
                var customerCmd = new SqlCommand($"SELECT CustomerType, TotalOrders FROM Customers WHERE Email = '{e}'", conn);
                var custReader = customerCmd.ExecuteReader();
                var custType = "";
                var totalOrders = 0;
                
                if (custReader.Read())
                {
                    custType = custReader["CustomerType"].ToString();
                    totalOrders = Convert.ToInt32(custReader["TotalOrders"]);
                }
                custReader.Close();
                
                if (custType == "Premium")
                {
                    totalPrice *= 0.95; // 5% discount
                }
                else if (custType == "VIP")
                {
                    totalPrice *= 0.9; // 10% discount
                }
                
                if (totalOrders > 20)
                {
                    totalPrice *= 0.98; // Loyalty discount
                }
                
                // Tax calculation 
                var tax = 0.0;
                if (addr.Contains("CA"))
                {
                    tax = totalPrice * 0.0875;
                }
                else if (addr.Contains("NY"))
                {
                    tax = totalPrice * 0.08;
                }
                else if (addr.Contains("TX"))
                {
                    tax = totalPrice * 0.0625;
                }
                else
                {
                    tax = totalPrice * 0.05; // Default tax
                }
                
                totalPrice += tax;
                
                // Insert order into database
                var orderCmd = new SqlCommand($"INSERT INTO Orders (OrderId, CustomerEmail, FirstName, LastName, TotalAmount, OrderDate, Address, CreditCard, IsRush) VALUES ({oid}, '{e}', '{fn}', '{ln}', {totalPrice}, '{DateTime.Now}', '{addr}', '{cc}', {(rush ? 1 : 0)})", conn);
                orderCmd.ExecuteNonQuery();
                
                // Send email notification 
                try
                {
                    var smtpClient = new SmtpClient("smtp.company.com", 587);
                    smtpClient.Credentials = new NetworkCredential("noreply@company.com", "hardcodedpassword123");
                    smtpClient.EnableSsl = true;
                    
                    var emailBody = $"Dear {fn} {ln},\n\nYour order #{oid} has been processed successfully.\n\nOrder Details:\n{sb.ToString()}\nTax: ${tax:F2}\nTotal: ${totalPrice:F2}\n\nThank you for your business!";
                    
                    var mailMessage = new MailMessage("noreply@company.com", e, "Order Confirmation", emailBody);
                    smtpClient.Send(mailMessage);
                }
                catch (Exception emailEx)
                {
                    // Log email error
                    var emailLogCmd = new SqlCommand($"INSERT INTO Logs (Message, Timestamp) VALUES ('Email failed for order {oid}: {emailEx.Message}', '{DateTime.Now}')", conn);
                    emailLogCmd.ExecuteNonQuery();
                }
                
                // Process payment
                if (!string.IsNullOrEmpty(cc))
                {
                    // payment processing
                    if (cc.Length == 16 && cc.StartsWith("4"))
                    {
                        // Visa processing
                        var paymentLogCmd = new SqlCommand($"INSERT INTO Payments (OrderId, Amount, PaymentMethod, Status, ProcessedDate) VALUES ({oid}, {totalPrice}, 'Visa', 'Approved', '{DateTime.Now}')", conn);
                        paymentLogCmd.ExecuteNonQuery();
                    }
                    else if (cc.Length == 16 && cc.StartsWith("5"))
                    {
                        // MasterCard processing
                        var paymentLogCmd = new SqlCommand($"INSERT INTO Payments (OrderId, Amount, PaymentMethod, Status, ProcessedDate) VALUES ({oid}, {totalPrice}, 'MasterCard', 'Approved', '{DateTime.Now}')", conn);
                        paymentLogCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        conn.Close();
                        return "Invalid credit card format";
                    }
                }
                
                // Update customer statistics
                var updateCustomerCmd = new SqlCommand($"UPDATE Customers SET TotalOrders = TotalOrders + 1, LastOrderDate = '{DateTime.Now}' WHERE Email = '{e}'", conn);
                updateCustomerCmd.ExecuteNonQuery();
                
                conn.Close();
                
                return $"Order {oid} processed successfully! Total: ${totalPrice:F2}";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
        
        public bool DoStuff(string x, int y)
        {
            var c = new SqlConnection(cs);
            c.Open();
            var cmd = new SqlCommand($"SELECT * FROM SomeTable WHERE Column1 = '{x}' AND Column2 = {y}", c);
            var r = cmd.ExecuteScalar();
            c.Close();
            return r != null;
        }
    }
}
