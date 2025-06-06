int x = 0;
x = x + 1;
int y = 0;

for(int i=0; i<10; i=++i){
    x = x + 1;
    if (x > 10)
    {
        y = x * x++;
    }
}

x;
y;
x++;
y--;
