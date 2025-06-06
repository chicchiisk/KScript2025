int x = 10;
int y = 5;

if (x > y) {
    x = x + y;
}

for (int i = 0; i < 3; i = i + 1) {
    y = y + i;
    x = x - 1;
}

int result = x * y;
result;