__put('H');
__put('e');
__put('l');
__put('l');
__put('o');
__put('\n');

char c = 'A';
__put(c);
__put('\n');

char d = 'Z';
__put(d);
__put('\n');

int ascii_A = 65;
__put(ascii_A);
__put('\n');

int ascii_B = 66;
__put(ascii_B);
__put('\n');

char[] message = {'W', 'o', 'r', 'l', 'd'};
__put(message[0]);
__put(message[1]);
__put(message[2]);
__put(message[3]);
__put(message[4]);
__put('\n');

void printChar(char ch) {
    __put(ch);
}

printChar('X');
printChar('Y');
printChar('Z');
__put('\n');

void printMessage() {
    __put('O');
    __put('K');
    __put('\n');
}

printMessage();

char getNextChar(char c) {
    return c + 1;
}

char next = getNextChar('M');
__put(next);
__put('\n');

char prev = getNextChar('P');
__put(prev);
__put('\n');

void printDigit(int digit) {
    char digitChar = '0' + digit;
    __put(digitChar);
}

printDigit(1);
printDigit(2);
printDigit(3);
__put('\n');

int number = 5;
char numberChar = '0' + number;
__put(numberChar);
__put('\n');

bool flag = true;
if (flag) {
    __put('T');
    __put('r');
    __put('u');
    __put('e');
} else {
    __put('F');
    __put('a');
    __put('l');
    __put('s');
    __put('e');
}
__put('\n');

struct Printer {
    char prefix;
    
    Printer() {
        prefix = '>';
    }
    
    void print(char c) {
        __put(prefix);
        __put(c);
        __put('\n');
    }
}

Printer p = new Printer();
p.print('A');
p.print('B');

p.prefix = '*';
p.print('C');

void printArray(char[] arr, int size) {
    int i = 0;
    if (i < size) { __put(arr[i]); i = i + 1; }
    if (i < size) { __put(arr[i]); i = i + 1; }
    if (i < size) { __put(arr[i]); i = i + 1; }
    if (i < size) { __put(arr[i]); i = i + 1; }
    if (i < size) { __put(arr[i]); }
    __put('\n');
}

char[] data = {'D', 'A', 'T', 'A'};
printArray(data, 4);

void recursivePrint(int count) {
    if (count > 0) {
        __put('*');
        recursivePrint(count - 1);
    }
}

recursivePrint(5);
__put('\n');

int getValue() {
    return 7;
}

int val = getValue();
char valChar = '0' + val;
__put(valChar);
__put('\n');

void printRange(int start, int end) {
    int current = start;
    if (current <= end) { __put('0' + current); current = current + 1; }
    if (current <= end) { __put('0' + current); current = current + 1; }
    if (current <= end) { __put('0' + current); current = current + 1; }
    __put('\n');
}

printRange(1, 3);

char space = ' ';
char exclamation = '!';
__put('D');
__put('o');
__put('n');
__put('e');
__put(space);
__put('T');
__put('e');
__put('s');
__put('t');
__put('i');
__put('n');
__put('g');
__put(exclamation);
__put('\n');