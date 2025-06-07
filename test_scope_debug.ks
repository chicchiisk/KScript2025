// Test variable scope in for loop
{
    int i = 0;
    i++;
    i; // Should be 1 and int type
    
    string test = "Hi";
    char c = charAt(test, i);
    __put(c);
}