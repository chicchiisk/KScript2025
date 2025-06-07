// Comprehensive test of all language improvements

// 1. Test comments (now supported)
// This is a single-line comment
int x = 5; // End-of-line comment
x;

// 2. Test while loops (now supported) 
int count = 0;
while (count < 3) {
    count = count + 1; // Using explicit increment to avoid type issue
}
count; // Should be 3

// 3. Test increment operators (mostly working, with minor type quirks)
int a = 10;
a++; // Postfix increment (no output due to expression statement filtering)
a; // Should be 11

int b = 20;  
++b; // Prefix increment (no output due to expression statement filtering)
b; // Should be 21

// 4. Test print function with while loops
import "test_script/lib/io.ks";
println("Testing print function:");
print("Hello");
println(" World!");

// 5. Test assignment not printing (fixed)
int silent = 42; // This assignment shouldn't print

// Print the final result
silent;