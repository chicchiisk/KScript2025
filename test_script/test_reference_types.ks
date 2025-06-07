class A
{
    int x;
    int y;
}
void B(A a)
{
    a.x=10;
    a.y=20;
}

A a = new A();
a.x;
a.y;
B(a);
a.x;
a.y;

__put('e');
__put('\n');


struct Point {
    int x;
    int y;
    
    Point() {
        x = 0;
        y = 0;
    }
    
    void move(int dx, int dy) {
        x = x + dx;
        y = y + dy;
    }
}
void P(Point p){
    p.x = 10;
    p.y = 20;

}

Point p = new Point();
p.x;
p.y;
P(p);
p.x;
p.y;

__put('e');
__put('\n');

class Person {
    int age;
    char name;
    Point position;
    
    Person() {
        age = 0;
        name = 'X';
        position = new Point();
        position.x=90;
        position.y=30;
    }
    
    void setAge(int newAge) {
        age = newAge;
    }
    
    int getAge() {
        return age;
    }
}

void ChangePerson(Person p)
{
    p.age=100;
    p.name="A";
    p.position.x=110;
    p.position.y=120;
}

Point structPoint = new Point();
structPoint.x;
structPoint.y;

structPoint.move(5, 10);
structPoint.x;
structPoint.y;

Person classPerson = new Person();
classPerson.age;
classPerson.name;
classPerson.position.x;
classPerson.position.y;

classPerson.setAge(25);
classPerson.getAge();

__put('W');

ChangePerson(classPerson);

class Container {
    Person person;
    Point point;
    
    Container() {
        person = new Person();
        point = new Point();
    }
    
    Person getPerson() {
        return person;
    }
}

Container container = new Container();
Person p = container.getPerson();
p.getAge();