import "utils_module.ks";

int result1 = add(5, 3);
result1;

int result2 = multiply(4, 6);
result2;

int result3 = subtract(10, 3);
result3;

float result4 = divide(8.0f, 2.0f);
result4;

PI;
MAX_VALUE;

import "geometry_module.ks";

Point p1 = new Point();
p1.getX();
p1.getY();

p1.setValues(10, 20);
p1.getX();
p1.getY();

int distance_from_origin = p1.getDistanceFromOrigin();
distance_from_origin;

Point p2 = createPoint(5, 15);
p2.getX();
p2.getY();

int simple_sum = simpleAdd(p1.getX(), p2.getY());
simple_sum;

Circle c1 = new Circle();
c1.radius;

c1.setRadius(5);
int area1 = c1.getArea();
area1;

Circle c2 = createCircle(3);
int area2 = c2.getArea();
area2;

import "data_module.ks";

NUMBER1;
NUMBER2;
NUMBER3;

Container container = createContainer(42);
int containerValue = container.getValue();
containerValue;

container.setValue(100);
int newValue = container.getValue();
newValue;

bool positive1 = isPositive(5);
bool positive2 = isPositive(-3);
bool positive3 = isPositive(0);
positive1;
positive2;
positive3;

char firstLetter = getFirstLetter();
firstLetter;

int combined_calculation = add(multiply(3, 4), subtract(20, 5));
combined_calculation;

Point p3 = createPoint(1, 2);
int final_sum = simpleAdd(p3.getX(), p3.getY());
final_sum;

Circle c3 = createCircle(2);
int final_area = c3.getArea();
final_area;