// Comprehensive increment/decrement test

// Test postfix increment
int a = 5;
int b = a++; // b should be 5, a should be 6
a; // Should print 6
b; // Should print 5

// Test prefix increment  
int c = 10;
int d = ++c; // c should be 11, d should be 11
c; // Should print 11
d; // Should print 11

// Test postfix decrement
int e = 8;
int f = e--; // f should be 8, e should be 7
e; // Should print 7
f; // Should print 8

// Test prefix decrement
int g = 15;
int h = --g; // g should be 14, h should be 14
g; // Should print 14
h; // Should print 14

// Test in loop context
int i = 0;
while (i < 3) {
    i++;
}
i; // Should print 3