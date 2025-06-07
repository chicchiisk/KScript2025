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

struct Rectangle {
    Point topLeft;
    Point bottomRight;
    
    Rectangle() {
        topLeft = new Point();
        bottomRight = new Point();
        topLeft.setValues(10, 20);
        bottomRight.setValues(50, 80);
    }
    
    int getWidth() {
        return bottomRight.getX() - topLeft.getX();
    }
    
    int getHeight() {
        return bottomRight.getY() - topLeft.getY();
    }
    
    Point getTopLeft() {
        return topLeft;
    }
}

Rectangle rect = new Rectangle();

int width = rect.getWidth();
width;

int height = rect.getHeight();
height;

Point tl = rect.getTopLeft();
int tlX = tl.getX();
tlX;

int tlY = tl.getY();
tlY;