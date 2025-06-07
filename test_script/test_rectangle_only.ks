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
}

struct Rectangle {
    Point topLeft;
    Point bottomRight;
    
    Rectangle() {
        topLeft = new Point();
        bottomRight = new Point();
    }
}