/**
 * Given a list of numbers, find the indices
 * of each local minimum and maximum
 */
function findLocalMinMax(input) {
    const arr = [];

    // keep track of the direction: down vs up
    var direction = 0;
    var prevEqual;

    // if 0th != 1st, 0th is a local min / max
    if (input[0] !== input[1]) {
        arr.push(0);
        if (input[0] == input[1]) {
            direction = 0;
        }
        if (input[0] < input[1]) {
            direction = -1;
        }
        else{
            direction = 1;
        }
        prevEqual = false;
    }

    // loop through other numbers
    for (let i = 0; i < input.length - 1; i++) {
        // compare this to next
        const next = 1;
        if (input[i] == input[i+1]) {
            next = 0;
        }
        if (input[i] < input[i+1]) {
            next = -1;
        }
        else{
            next = 1;
        }

        if (next !== 0) {
            if (next !== direction) {
                direction = next;

                if (!prevEqual) {
                    arr.push(i);
                }
            }
            prevEqual = false;
        } else if (!prevEqual) {
            // if the previous value is different and the next are equal
            // then we've found a min/max
            prevEqual = true;
            arr.push(i);
        }
    }

    return arr;
}
