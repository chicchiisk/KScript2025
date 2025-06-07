// Test while loop functionality

// Simple counting loop
int count = 0;
while (count < 5) {
    count; // Should print 0, 1, 2, 3, 4
    count = count + 1;
}

// Test with string iteration (rewrite print function using while)
string message = "Hello";
int i = 0;
while (i < length(message)) {
    char c = charAt(message, i);
    __put(c);
    i = i + 1;
}
__put('\n'); // Add newline

// Test boolean condition
bool flag = true;
int counter = 0;
while (flag) {
    counter = counter + 1;
    if (counter > 3) {
        flag = false;
    }
}
counter; // Should be 4