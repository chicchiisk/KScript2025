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
}

export Point createPoint(int x, int y) {
    Point p = new Point();
    p.setValues(x, y);
    return p;
}