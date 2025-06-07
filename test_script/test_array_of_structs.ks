struct Point {
    int x;
    int y;
    
    Point() {
        x = 0;
        y = 0;
    }
    
    void setValues(int newX, int newY) {
        x = newX;
        y = newY;
    }
    
    int getX() {
        return x;
    }
    
    int getY() {
        return y;
    }
}

Point[] points = new Point[3];

points[0] = new Point();
points[1] = new Point();
points[2] = new Point();

points[0].setValues(10, 20);
points[1].setValues(30, 40);
points[2].setValues(50, 60);

int x0 = points[0].getX();
x0;

int y0 = points[0].getY();
y0;

int x1 = points[1].getX();
x1;

int y1 = points[1].getY();
y1;

int x2 = points[2].getX();
x2;

int y2 = points[2].getY();
y2;