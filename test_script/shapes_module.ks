export struct Point {
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

export struct Rectangle {
    Point topLeft;
    Point bottomRight;
    
    Rectangle() {
        topLeft = new Point();
        bottomRight = new Point();
    }
    
    void setBounds(int x1, int y1, int x2, int y2) {
        topLeft.setValues(x1, y1);
        bottomRight.setValues(x2, y2);
    }
    
    int getArea() {
        int width = bottomRight.getX() - topLeft.getX();
        int height = bottomRight.getY() - topLeft.getY();
        return width * height;
    }
}

export Point createPoint(int x, int y) {
    Point p = new Point();
    p.setValues(x, y);
    return p;
}