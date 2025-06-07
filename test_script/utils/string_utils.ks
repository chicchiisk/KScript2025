export void printHello() {
    __put('H');
    __put('e');
    __put('l');
    __put('l');
    __put('o');
    __put('\n');
}

export char toUpper(char c) {
    if (c >= 'a' && c <= 'z') {
        return c - 32;
    }
    return c;
}