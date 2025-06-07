struct Point {
    int x;
    int y;
}

Point p1 = new Point();
p1.x;
p1.y;

p1.x = 10;
p1.y = 20;
p1.x;
p1.y;

struct Rectangle {
    int width;
    int height;
    
    Rectangle() {
        width = 5;
        height = 3;
    }
    
    int getArea() {
        return width * height;
    }
    
    int getPerimeter() {
        return 2 * (width + height);
    }
}

Rectangle r1 = new Rectangle();
r1.width;
r1.height;

int area1 = r1.getArea();
area1;

int perimeter1 = r1.getPerimeter();
perimeter1;

r1.width = 8;
r1.height = 6;

int area2 = r1.getArea();
area2;

int perimeter2 = r1.getPerimeter();
perimeter2;

struct Circle {
    float radius;
    
    Circle() {
        radius = 1.0f;
    }
    
    float getArea() {
        return 3.14f * radius * radius;
    }
    
    float getDiameter() {
        return 2.0f * radius;
    }
}

Circle c1 = new Circle();
c1.radius;

float circleArea = c1.getArea();
circleArea;

float diameter = c1.getDiameter();
diameter;

c1.radius = 2.5f;
float newArea = c1.getArea();
newArea;

float newDiameter = c1.getDiameter();
newDiameter;

struct Person {
    int age;
    char initial;
    bool isAdult;
    
    Person() {
        age = 0;
        initial = 'X';
        isAdult = false;
    }
    
    void updateAge(int newAge) {
        age = newAge;
        if (age >= 18) {
            isAdult = true;
        } else {
            isAdult = false;
        }
    }
    
    char getInitial() {
        return initial;
    }
}

Person p = new Person();
p.age;
p.initial;
p.isAdult;

p.updateAge(25);
p.age;
p.isAdult;

p.initial = 'J';
char personInitial = p.getInitial();
personInitial;

struct Container {
    Point topLeft;
    Point bottomRight;
    
    Container() {
        topLeft = new Point();
        bottomRight = new Point();
        topLeft.x = 0;
        topLeft.y = 0;
        bottomRight.x = 10;
        bottomRight.y = 10;
    }
    
    int getWidth() {
        return bottomRight.x - topLeft.x;
    }
    
    int getHeight() {
        return bottomRight.y - topLeft.y;
    }
}

Container container = new Container();
container.topLeft.x;
container.topLeft.y;
container.bottomRight.x;
container.bottomRight.y;

int containerWidth = container.getWidth();
containerWidth;

int containerHeight = container.getHeight();
containerHeight;

container.topLeft.x = 5;
container.topLeft.y = 5;
container.bottomRight.x = 20;
container.bottomRight.y = 15;

int newWidth = container.getWidth();
newWidth;

int newHeight = container.getHeight();
newHeight;

Point[] points = new Point[3];
points[0] = new Point();
points[1] = new Point();
points[2] = new Point();

points[0].x = 1;
points[0].y = 2;
points[1].x = 3;
points[1].y = 4;
points[2].x = 5;
points[2].y = 6;

points[0].x;
points[0].y;
points[1].x;
points[1].y;
points[2].x;
points[2].y;

Rectangle[] rectangles = new Rectangle[2];
rectangles[0] = new Rectangle();
rectangles[1] = new Rectangle();

rectangles[0].width = 4;
rectangles[0].height = 3;
rectangles[1].width = 6;
rectangles[1].height = 8;

int rectArea1 = rectangles[0].getArea();
int rectArea2 = rectangles[1].getArea();
rectArea1;
rectArea2;

struct Calculator {
    int lastResult;
    
    Calculator() {
        lastResult = 0;
    }
    
    int add(int a, int b) {
        lastResult = a + b;
        return lastResult;
    }
    
    int multiply(int a, int b) {
        lastResult = a * b;
        return lastResult;
    }
    
    int getLastResult() {
        return lastResult;
    }
}

Calculator calc = new Calculator();
calc.lastResult;

int sum = calc.add(10, 5);
sum;

int lastAfterAdd = calc.getLastResult();
lastAfterAdd;

int product = calc.multiply(4, 6);
product;

int lastAfterMultiply = calc.getLastResult();
lastAfterMultiply;

struct Counter {
    int count;
    
    Counter() {
        count = 0;
    }
    
    void increment() {
        count++;
    }
    
    void decrement() {
        count--;
    }
    
    int getValue() {
        return count;
    }
    
    void reset() {
        count = 0;
    }
}

Counter counter = new Counter();
int initialCount = counter.getValue();
initialCount;

counter.increment();
counter.increment();
counter.increment();

int afterIncrements = counter.getValue();
afterIncrements;

counter.decrement();
int afterDecrement = counter.getValue();
afterDecrement;

counter.reset();
int afterReset = counter.getValue();
afterReset;

struct ComplexStruct {
    int intField;
    float floatField;
    char charField;
    bool boolField;
    Point pointField;
    
    ComplexStruct() {
        intField = 42;
        floatField = 3.14f;
        charField = 'C';
        boolField = true;
        pointField = new Point();
        pointField.x = 100;
        pointField.y = 200;
    }
    
    int getAllFieldsSum() {
        int boolValue = 0;
        if (boolField) {
            boolValue = 1;
        }
        return intField + floatField + charField + boolValue + pointField.x + pointField.y;
    }
}

ComplexStruct complex = new ComplexStruct();
complex.intField;
complex.floatField;
complex.charField;
complex.boolField;
complex.pointField.x;
complex.pointField.y;

int totalSum = complex.getAllFieldsSum();
totalSum;