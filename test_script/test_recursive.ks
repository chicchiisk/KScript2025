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

int fact5 = factorial(5);
fact5;

int fib7 = fibonacci(7);
fib7;

int countdown(int n) {
    if (n <= 0) {
        return 0;
    }
    return countdown(n - 1);
}

int result = countdown(3);
result;