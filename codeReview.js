/*
* Given a list of numbers, find the indices
* of each local minimum and maximum (x1, x2, x3, x4)
* If the first point or last point in the graph are not equal to their neighbor,
* they are a local min or max
* Saddle points are a min/max, but the middle values in a series of equal numbers
* are not mins/maxes
*/
function findLocalMinMax(inputArr) {
   const minsAndMaxes = [];

   // keep track of the direction: down vs up
   let currDirection, nextDirection, prevEqual;
   if(inputArr.length < 2){
       return inputArr;
   }
  
   currDirection = determineDirection(inputArr[0], inputArr[1]);

   // if 0th != 1st, 0th is a local min / max
   if (currDirection !== Direction.FLAT) {
       minsAndMaxes.push(0);
       prevEqual = false;
   }

   // loop through other numbers
   for (let i = 1; i < inputArr.length - 1; i++) {
       // compare this to next to determine direction of next item
       nextDirection = determineDirection(inputArr[i], inputArr[i+1])

       if (nextDirection !=== currDirection) {
           minsAndMaxes.push(i);
       }
   }

   // Determine if last element is local min/max
   if (nextDirection !== Direction.FLAT)
       minsAndMaxes.push(i)
   }

   return minsAndMaxes;
}

function determineDirection(a, b) {
   return (a === b ? Direction.FLAT :
       a < b ? Direction.INCREASING : Direction.DECREASING;
}



const Direction = Object.freeze({
   INCREASING: 1,
   FLAT: 0,
   DECREASING: -1,
});

