struct Point {
    int x;
    int y;
    
    Point() {
        x = 0;
        y = 0;
    }
    
    int getX() {
        return x;
    }
    
    int getY() {
        return y;
    }
}

struct Rectangle {
    Point topLeft;
    Point bottomRight;
    
    Rectangle() {
        topLeft = new Point();
        bottomRight = new Point();
    }
    
    int getWidth() {
        return bottomRight.getX() - topLeft.getX();
    }
}