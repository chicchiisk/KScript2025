// Check variable types
for (int i = 0; i < 2; i++) {
    // Test charAt with direct i
    string test = "Hi";
    if (i < length(test)) {
        char c = charAt(test, i);
        __put(c);
    }
}
__put('\n');