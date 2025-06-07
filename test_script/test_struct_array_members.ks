struct Vector {
    int[] data;
    int size;
    
    Vector() {
        data = new int[3];
        size = 3;
        data[0] = 1;
        data[1] = 2;
        data[2] = 3;
    }
    
    int get(int index) {
        return data[index];
    }
    
    void set(int index, int value) {
        data[index] = value;
    }
    
    int getSize() {
        return size;
    }
}

Vector v = new Vector();

int size = v.getSize();
size;

int first = v.get(0);
first;

int second = v.get(1);
second;

int third = v.get(2);
third;

v.set(1, 99);

int modifiedSecond = v.get(1);
modifiedSecond;