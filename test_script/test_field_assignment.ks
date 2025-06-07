import "point_module.ks";

Point p = new Point();

int originalX = p.getX();
originalX;

int originalY = p.getY();  
originalY;

p.x = 100;
p.y = 200;

int newX = p.getX();
newX;

int newY = p.getY();
newY;

Point p2 = new Point();
p2.x = p.x + 50;
p2.y = p.y + 75;

int finalX = p2.getX();
finalX;

int finalY = p2.getY();
finalY;