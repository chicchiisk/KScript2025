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
}

struct Rectangle {
    Point topLeft;
    
    Rectangle() {
        topLeft = new Point();
    }
    
    Point getTopLeft() {
        return topLeft;
    }
}