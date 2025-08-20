#include "pch.h"
#include "extensions.h"
#include <string.h>
#include <cmath>

const double pi = 3.14159265358979323846;

EXTENSIONS int levenshtein_distance(const char* str1, const char* str2)
{
    int len1 = strlen(str1);
    int len2 = strlen(str2);

    if (len1 == 0)
        return len2;
    if (len2 == 0)
        return len1;

    // Create a matrix to store the distances
    int** matrix = new int* [len1 + 1];
    for (int i = 0; i <= len1; i++)
        matrix[i] = new int[len2 + 1];

    // Initialize the matrix
    for (int i = 0; i <= len1; i++)
        matrix[i][0] = i;
    for (int j = 0; j <= len2; j++)
        matrix[0][j] = j;

    // Fill the matrix with the Levenshtein distance values
    for (int i = 1; i <= len1; i++)
    {
        for (int j = 1; j <= len2; j++)
        {
            int cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;

            matrix[i][j] = fmin(fmin(matrix[i - 1][j] + 1, matrix[i][j - 1] + 1), matrix[i - 1][j - 1] + cost);
        }
    }

    // Return the Levenshtein distance from the bottom-right corner of the matrix
    return matrix[len1][len2];
}

EXTENSIONS void levenshtein_function(sqlite3_context* context, int argc, sqlite3_value** argv)
{
    if (argc == 2 && sqlite3_value_type(argv[0]) == SQLITE_TEXT && sqlite3_value_type(argv[1]) == SQLITE_TEXT)
    {
        const char* str1 = (const char*)sqlite3_value_text(argv[0]);
        const char* str2 = (const char*)sqlite3_value_text(argv[1]);

        // Compute the Levenshtein distance between str1 and str2
        int dist = levenshtein_distance(str1, str2);

        // Return the distance as an integer
        sqlite3_result_int(context, dist);
    }
    else
    {
        // Return NULL if the arguments are invalid
        sqlite3_result_null(context);
    }
}

EXTENSIONS double haversine_distance(double lat_a, double lon_a, double lat_b, double lon_b)
{
    //Check for same coordinate
    if (lat_a == lat_b && lon_a == lon_b)
        return 0;

    //Check for poles
    double pi2 = pi / 2;
    if ((lat_a == pi2 && lat_b == pi2) || (lat_a == -pi2 && lat_b == -pi2))
        return 0;

    //Convert to rectangular coordinates
    double a[] =
    {
        sin(lon_a) * cos(lat_a),
        sin(lat_a),
        cos(lon_a) * cos(lat_a)
    };

    double b[] =
    {
        sin(lon_b) * cos(lat_b),
        sin(lat_b),
        cos(lon_b) * cos(lat_b)
    };

    //Calculate the magnitude of the vectors
    double ma = sqrt(a[0] * a[0] + a[1] * a[1] + a[2] * a[2]);
    double mb = sqrt(b[0] * b[0] + b[1] * b[1] + b[2] * b[2]);

    //Convert to unit vectors
    a[0] /= ma;
    a[1] /= ma;
    a[2] /= ma;

    b[0] /= mb;
    b[1] /= mb;
    b[2] /= mb;

    //Take the dot product
    return acos(a[0] * b[0] + a[1] * b[1] + a[2] * b[2]);
}

EXTENSIONS void haversine_function(sqlite3_context* context, int argc, sqlite3_value** argv)
{
    if (argc == 4 &&
        sqlite3_value_type(argv[0]) == SQLITE_FLOAT &&
        sqlite3_value_type(argv[1]) == SQLITE_FLOAT &&
        sqlite3_value_type(argv[2]) == SQLITE_FLOAT &&
        sqlite3_value_type(argv[3]) == SQLITE_FLOAT)
    {
        float latA = sqlite3_value_double(argv[0]);
        float lonA = sqlite3_value_double(argv[1]);
        float latB = sqlite3_value_double(argv[2]);
        float lonB = sqlite3_value_double(argv[3]);

        // Compute the Levenshtein distance between str1 and str2
        double dist = haversine_distance(latA, lonA, latB, lonB);

        // Return the distance as an integer
        sqlite3_result_double(context, dist);
    }
    else
    {
        // Return NULL if the arguments are invalid
        sqlite3_result_null(context);
    }
}

EXTENSIONS double radToDist(double rad)
{
    return rad * (double)10800 / pi;
}


EXTENSIONS void radToDist_function(sqlite3_context* context, int argc, sqlite3_value** argv)
{
    if (argc == 1 && sqlite3_value_type(argv[0]) == SQLITE_FLOAT)
    {
        double rad = sqlite3_value_double(argv[0]);

        // Compute the Levenshtein distance between str1 and str2
        double dist = radToDist(rad);

        // Return the distance as an integer
        sqlite3_result_double(context, dist);
    }
    else
    {
        // Return NULL if the arguments are invalid
        sqlite3_result_null(context);
    }
}

EXTENSIONS double distToRad(double dist)
{
    return dist * pi / (double)10800;
}


EXTENSIONS void distToRad_function(sqlite3_context* context, int argc, sqlite3_value** argv)
{
    if (argc == 1 && sqlite3_value_type(argv[0]) == SQLITE_FLOAT)
    {
        double dist = sqlite3_value_double(argv[0]);

        // Compute the Levenshtein distance between str1 and str2
        double rad = distToRad(dist);

        // Return the distance as an integer
        sqlite3_result_double(context, rad);
    }
    else
    {
        // Return NULL if the arguments are invalid
        sqlite3_result_null(context);
    }
}