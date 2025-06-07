// Debug increment type issue
int x = 5;
x++; // Should keep x as int
x; // Should be 6

// Test in for loop context
for (int y = 0; y < 2; y++) {
    y;
}