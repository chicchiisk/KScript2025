int x = 10;
float y = 3.14f;
char c = 'A';
bool b = true;

x;
y;
c;
b;

int sum = 5 + 3;
int diff = 10 - 4;
int prod = 6 * 7;
int quot = 15 / 3;
int mod = 17 % 5;

sum;
diff;
prod;
quot;
mod;

int a = 100;
int b1 = a++;
b1;
a;

int c1 = ++a;
c1;
a;

int d = a--;
d;
a;

int e = --a;
e;
a;

bool eq = (5 == 5);
bool neq = (5 != 3);
bool gt = (10 > 5);
bool gte = (10 >= 10);
bool lt = (3 < 7);
bool lte = (5 <= 5);

eq;
neq;
gt;
gte;
lt;
lte;

bool and1 = true && true;
bool and2 = true && false;
bool or1 = false || true;
bool or2 = false || false;

and1;
and2;
or1;
or2;

int intVal = 42;
float floatFromInt = intVal;
floatFromInt;

char ch = 'Z';
char nextChar = ch + 1;
char prevChar = ch - 1;
nextChar;
prevChar;

bool trueVal = true;
bool falseVal = false;
int boolToInt1 = trueVal;
int boolToInt2 = falseVal;
boolToInt1;
boolToInt2;

float mixedResult = intVal + floatFromInt;
mixedResult;

int add(int a, int b) {
    return a + b;
}

float calculateArea(float width, float height) {
    return width * height;
}

char toUpper(char c) {
    if (c >= 'a' && c <= 'z') {
        return c - 32;
    }
    return c;
}

bool isEven(int n) {
    return (n % 2) == 0;
}

int addResult = add(15, 25);
addResult;

float area = calculateArea(5.5f, 3.2f);
area;

char upperA = toUpper('a');
char upperZ = toUpper('Z');
upperA;
upperZ;

bool even4 = isEven(4);
bool even7 = isEven(7);
even4;
even7;

int loopSum = 0;
for (int i = 1; i <= 5; i++) {
    loopSum = loopSum + i;
}
loopSum;

int counter = 0;
for (int j = 0; j < 3; j++) {
    counter++;
}
counter;

int testValue = 42;
int result = 0;

if (testValue > 50) {
    result = 1;
} else if (testValue > 30) {
    result = 2;
} else {
    result = 3;
}
result;

int nestedResult = 0;
for (int k = 0; k < 3; k++) {
    if (k % 2 == 0) {
        nestedResult = nestedResult + k;
    } else {
        nestedResult = nestedResult + (k * 2);
    }
}
nestedResult;

int factorial(int n) {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}

int fibonacci(int n) {
    if (n <= 1) {
        return n;
    }
    return fibonacci(n - 1) + fibonacci(n - 2);
}

int countdown(int n) {
    if (n <= 0) {
        return 0;
    }
    return n + countdown(n - 1);
}

int fact5 = factorial(5);
fact5;

int fib6 = fibonacci(6);
fib6;

int countdownSum = countdown(4);
countdownSum;

int complexCalculation(int base, int exponent) {
    int result = 1;
    for (int i = 0; i < exponent; i++) {
        result = result * base;
    }
    return result;
}

int absoluteValue(int n) {
    if (n < 0) {
        return -n;
    }
    return n;
}

int power = complexCalculation(2, 8);
power;

int abs1 = absoluteValue(-15);
int abs2 = absoluteValue(25);
abs1;
abs2;

int zero = 0;
int divByOne = zero / 1;
int modByOne = zero % 1;
divByOne;
modByOne;

char minChar = 'A';
char maxChar = 'Z';
int charDiff = maxChar - minChar;
charDiff;

bool nonZeroTrue = 5 && 3;
bool zeroFalse = 0 || 0;
nonZeroTrue;
zeroFalse;

int factResult = factorial(4);
int powResult = complexCalculation(absoluteValue(-3), 2);
int addResult2 = add(factResult, powResult);

int bonusPoints = 0;
if (isEven(8)) {
    bonusPoints = 10;
} else {
    bonusPoints = 5;
}

int finalResult = addResult2 + bonusPoints;
finalResult;