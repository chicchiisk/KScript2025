struct Point {
    int x;
    int y;
}

struct Container {
    Point p;
    
    Container() {
        p = new Point();
    }
}

Container c = new Container();