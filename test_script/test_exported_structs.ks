import "shapes_module.ks";

Point p1 = new Point();
p1.setValues(5, 10);

int x1 = p1.getX();
x1;

int y1 = p1.getY();
y1;

Rectangle rect = new Rectangle();
rect.setBounds(0, 0, 100, 50);

int area = rect.getArea();
area;

Point p2 = createPoint(25, 75);
int x2 = p2.getX();
x2;

int y2 = p2.getY();
y2;