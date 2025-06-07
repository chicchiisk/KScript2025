export int NUMBER1 = 10;
export int NUMBER2 = 20;
export int NUMBER3 = 30;

export struct Container {
    int value;
    
    Container() {
        value = 0;
    }
    
    void setValue(int v) {
        value = v;
    }
    
    int getValue() {
        return value;
    }
}

export Container createContainer(int initialValue) {
    Container c = new Container();
    c.setValue(initialValue);
    return c;
}

export bool isPositive(int n) {
    return n > 0;
}

export char getFirstLetter() {
    return 'A';
}