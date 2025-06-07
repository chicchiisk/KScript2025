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
    
    int getDistanceFromOrigin() {
        return x + y;
    }
}

export struct Circle {
    int radius;
    
    Circle() {
        radius = 1;
    }
    
    void setRadius(int r) {
        radius = r;
    }
    
    int getArea() {
        return 3 * radius * radius;
    }
}

export Point createPoint(int x, int y) {
    Point p = new Point();
    p.setValues(x, y);
    return p;
}

export Circle createCircle(int radius) {
    Circle c = new Circle();
    c.setRadius(radius);
    return c;
}

export int simpleAdd(int a, int b) {
    return a + b;
}