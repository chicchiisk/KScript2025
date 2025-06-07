struct Point {
    int x;
    int y;
    
    Point() {
        x = 3;
        y = 4;
    }
    
    int getX() {
        return x;
    }
    
    int getY() {
        return y;
    }
    
    void setValues(int newX, int newY) {
        x = newX;
        y = newY;
    }
}

Point p = new Point();
int originalX = p.getX();
originalX;

int originalY = p.getY();
originalY;

p.setValues(7, 8);

int newX = p.getX();
newX;

int newY = p.getY();
newY;