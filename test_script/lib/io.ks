// I/O library for debugging and output functions

// Print function that outputs a string to standard output
export void print(string message) {
    // Iterate through each character in the string and output using __put
    int i = 0;
    while (i < length(message)) {
        char c = charAt(message, i);
        __put(c);
        i = i + 1;
    }
}

// Print function with newline
export void println(string message) {
    print(message);
    __put('\n'); // Add newline character
}