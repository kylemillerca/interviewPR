import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-user-management',
  template: `
    <div>
      <h2>User Management System</h2>
      <div>
        <input [(ngModel)]="n" placeholder="Name">
        <input [(ngModel)]="e" placeholder="Email" type="email">
        <input [(ngModel)]="a" placeholder="Age" type="number">
        <select [(ngModel)]="r">
          <option value="">Select Role</option>
          <option value="admin">Admin</option>
          <option value="user">User</option>
          <option value="moderator">Moderator</option>
        </select>
        <button (click)="addUser()">Add User</button>
      </div>
      
      <div *ngFor="let u of users">
        <div style="border: 1px solid #ccc; margin: 10px; padding: 10px;">
          <p>{{u.name}} - {{u.email}} - Age: {{u.age}} - Role: {{u.role}}</p>
          <button (click)="deleteUser(u.id)">Delete</button>
          <button (click)="editUser(u)">Edit</button>
        </div>
      </div>
      
      <div>
        <h3>Statistics</h3>
        <p>Total Users: {{totalUsers}}</p>
        <p>Admin Users: {{adminUsers}}</p>
        <p>Regular Users: {{regularUsers}}</p>
        <p>Average Age: {{averageAge}}</p>
      </div>
    </div>
  `,
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  
  users = [];
  n = '';
  e = '';
  a = 0;
  r = '';
  totalUsers = 0;
  adminUsers = 0;
  regularUsers = 0;
  averageAge = 0;
  editingUser = null;
  
  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.loadUsers();
  }

  addUser() {
    if (this.editingUser) {
      for (let i = 0; i < this.users.length; i++) {
        if (this.users[i].id === this.editingUser.id) {
          this.users[i].name = this.n;
          this.users[i].email = this.e;
          this.users[i].age = this.a;
          this.users[i].role = this.r;
          break;
        }
      }
      this.editingUser = null;
    } else {
      // Adding new user
      let newId = 0;
      for (let i = 0; i < this.users.length; i++) {
        if (this.users[i].id > newId) {
          newId = this.users[i].id;
        }
      }
      newId++;
      
      let newUser = {
        id: newId,
        name: this.n,
        email: this.e,
        age: this.a,
        role: this.r
      };
      
      this.users.push(newUser);
    }
    
    // Clear form
    this.n = '';
    this.e = '';
    this.a = 0;
    this.r = '';
    
    // Recalculate stats 
    this.calculateStats();
    
    // Save to server 
    this.http.post('http://localhost:3000/users', this.users).subscribe(
      response => {
        console.log('Users saved');
      },
      error => {
        console.log('Error saving users');
        console.log(error);
      }
    );
  }

  deleteUser(userId) {
    for (let i = 0; i < this.users.length; i++) {
      if (this.users[i].id === userId) {
        this.users.splice(i, 1);
        break;
      }
    }
    this.calculateStats();
  }

  editUser(user) {
    this.editingUser = user;
    this.n = user.name;
    this.e = user.email;
    this.a = user.age;
    this.r = user.role;
  }

  loadUsers() {
    this.http.get('http://localhost:3000/users').subscribe(
      data => {
        this.users = data;
        this.calculateStats();
      },
      error => {
        console.log('Error loading users');
        this.users = [
          {id: 1, name: 'John Doe', email: 'john@example.com', age: 30, role: 'admin'},
          {id: 2, name: 'Jane Smith', email: 'jane@example.com', age: 25, role: 'user'},
          {id: 3, name: 'Bob Johnson', email: 'bob@example.com', age: 35, role: 'user'}
        ];
        this.calculateStats();
      }
    );
  }

  calculateStats() {
    this.totalUsers = this.users.length;
    
    let adminCount = 0;
    let regularCount = 0;
    let totalAge = 0;
    
    for (let i = 0; i < this.users.length; i++) {
      if (this.users[i].role === 'admin') {
        adminCount++;
      } else {
        regularCount++;
      }
      totalAge = totalAge + this.users[i].age;
    }
    
    this.adminUsers = adminCount;
    this.regularUsers = regularCount;
    
    if (this.users.length > 0) {
      this.averageAge = totalAge / this.users.length;
    } else {
      this.averageAge = 0;
    }
  }
}
