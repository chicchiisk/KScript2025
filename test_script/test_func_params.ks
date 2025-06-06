float calculateArea(float width, float height) {
    return width * height;
}

bool isEven(int num) {
    int remainder = num % 2;
    return remainder == 0;
}

char nextChar(char c) {
    return c + 1;
}

float area = calculateArea(5.5f, 3.2f);
area;

bool even = isEven(42);
even;

bool odd = isEven(17);
odd;

char next = nextChar('A');
next;

int maxValue(int a, int b, int c) {
    if (a > b && a > c) {
        return a;
    }
    if (b > c) {
        return b;
    }
    return c;
}

int max = maxValue(10, 25, 8);
max;