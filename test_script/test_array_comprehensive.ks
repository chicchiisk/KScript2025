int[] numbers = new int[5];
numbers[0] = 10;
numbers[1] = 20;
numbers[2] = 30;
numbers[3] = 40;
numbers[4] = 50;

numbers[0];
numbers[1];
numbers[2];
numbers[3];
numbers[4];

int manual_sum = numbers[0] + numbers[1] + numbers[2] + numbers[3] + numbers[4];
manual_sum;

numbers[2] = numbers[2] + 5;
numbers[2];

float[] floats = {1.1f, 2.2f, 3.3f};
floats[0];
floats[1];
floats[2];

float float_sum = floats[0] + floats[1] + floats[2];
float_sum;

char[] chars = {'A', 'B', 'C', 'D'};
chars[0];
chars[1];
chars[2];
chars[3];

chars[1] = chars[1] + 1;
chars[1];

bool[] bools = {true, false, true};
bools[0];
bools[1];
bools[2];

int true_count = 0;
if (bools[0]) { true_count = true_count + 1; }
if (bools[1]) { true_count = true_count + 1; }
if (bools[2]) { true_count = true_count + 1; }
true_count;

int[] literalArray = {1, 2, 3, 4, 5};
literalArray[0];
literalArray[1];
literalArray[2];
literalArray[3];
literalArray[4];

int literal_sum = literalArray[0] + literalArray[1] + literalArray[2] + literalArray[3] + literalArray[4];
literal_sum;

int[,] matrix = new int[3, 3];
matrix[0, 0] = 1;
matrix[0, 1] = 2;
matrix[0, 2] = 3;
matrix[1, 0] = 4;
matrix[1, 1] = 5;
matrix[1, 2] = 6;
matrix[2, 0] = 7;
matrix[2, 1] = 8;
matrix[2, 2] = 9;

matrix[0, 0];
matrix[0, 1];
matrix[0, 2];
matrix[1, 0];
matrix[1, 1];
matrix[1, 2];
matrix[2, 0];
matrix[2, 1];
matrix[2, 2];

int matrix_sum = matrix[0, 0] + matrix[0, 1] + matrix[0, 2] + matrix[1, 0] + matrix[1, 1] + matrix[1, 2] + matrix[2, 0] + matrix[2, 1] + matrix[2, 2];
matrix_sum;

int diagonal = matrix[0, 0] + matrix[1, 1] + matrix[2, 2];
diagonal;

struct Point {
    int x;
    int y;
}

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

int points_sum_x = points[0].x + points[1].x + points[2].x;
points_sum_x;

int[] empty_array = new int[0];
int[] single_element = new int[1];
single_element[0] = 42;
single_element[0];

int[] copy_array = new int[3];
int[] original_array = {100, 200, 300};

copy_array[0] = original_array[0];
copy_array[1] = original_array[1];
copy_array[2] = original_array[2];

copy_array[0];
copy_array[1];
copy_array[2];

copy_array[2] = 999;
copy_array[2];
original_array[2];