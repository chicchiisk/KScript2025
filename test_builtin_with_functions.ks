void printString(char[] str) {
    int i = 0;
    while (i < 5) {
        __put(str[i]);
        i++;
    }
}

void printNumber(int n) {
    if (n >= 10) {
        printNumber(n / 10);
    }
    char digit = '0' + (n % 10);
    __put(digit);
}

char[] hello = {'H', 'e', 'l', 'l', 'o'};
printString(hello);
__put('\n');

printNumber(42);
__put('\n');

printNumber(123);
__put('\n');