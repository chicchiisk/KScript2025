void greet(char name) {
    __put('H');
    __put('i');
    __put(' ');
    __put(name);
    __put('!');
    __put('\n');
}

char getNextChar(char c) {
    return c + 1;
}

greet('A');
greet('B');

char next = getNextChar('X');
__put(next);
__put('\n');

int add(int a, int b) {
    return a + b;
}

int result = add(5, 3);
__put('0' + result);
__put('\n');