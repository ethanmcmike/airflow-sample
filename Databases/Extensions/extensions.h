// MathLibrary.h - Contains declarations of math functions
#pragma once
#include <sqlite3ext.h>
#include <string.h>
#include <iostream>
#include <stdio.h>
#include <ctype.h>

SQLITE_EXTENSION_INIT1

#ifdef EXTENSIONS_LIB
#define EXTENSIONS extern "C" __declspec(dllexport)
#else
#define EXTENSIONS extern "C" __declspec(dllimport)
#endif

/// <summary>
/// The SQL extension function for levenshtein distance
/// </summary>
/// <param name="context"></param>
/// <param name="argc"></param>
/// <param name="argv"></param>
/// <returns></returns>
EXTENSIONS void levenshtein_function(sqlite3_context* context, int argc, sqlite3_value** argv);

/// <summary>
/// Computes the Levenshtein distance between two strings
/// </summary>
/// <param name="str1">String 1</param>
/// <param name="str2">String 2</param>
/// <returns>The Levenshtein distance between the two given strings</returns>
EXTENSIONS int levenshtein_distance(const char* str1, const char* str2);

/// <summary>
/// The SQL extension function for haversine distance
/// </summary>
/// <param name="context"></param>
/// <param name="argc"></param>
/// <param name="argv"></param>
/// <returns></returns>
EXTENSIONS void haversine_function(sqlite3_context* context, int argc, sqlite3_value** argv);

/// <summary>
/// Computes the haversine distance between two coordinates
/// </summary>
/// <param name="lat_a">Latitude of coordinate A</param>
/// <param name="lon_a">Longitude of coordinate A</param>
/// <param name="lat_b">Latitude of coordinate B</param>
/// <param name="lon_b">Longitude of coordinate B</param>
/// <returns>The distance [radians] between the two coordinates</returns>
EXTENSIONS double haversine_distance(double lat_a, double lon_a, double lat_b, double lon_b);

/// <summary>
/// The SQL extension function for converting distance from radians to nautical miles
/// </summary>
/// <param name="context"></param>
/// <param name="argc"></param>
/// <param name="argv"></param>
/// <returns></returns>
EXTENSIONS void radToDist_function(sqlite3_context* context, int argc, sqlite3_value** argv);

/// <summary>
/// Converts distance in radians to distance in nautical miles
/// </summary>
/// <param name="rad">Distance in radians</param>
/// <returns>Distance in nautical miles</returns>
EXTENSIONS double radToDist(double rad);

/// <summary>
/// The SQL extension function for converting distance from nautical miles to radians
/// </summary>
/// <param name="context"></param>
/// <param name="argc"></param>
/// <param name="argv"></param>
/// <returns></returns>
EXTENSIONS void distToRad_function(sqlite3_context* context, int argc, sqlite3_value** argv);

/// <summary>
/// Converts distance from nautical miles to radians
/// </summary>
/// <param name="dist">Distance in nautical miles</param>
/// <returns>Distance in radians</returns>
EXTENSIONS double distToRad(double dist);

//Define the SQL function aliases
extern "C" __declspec(dllexport) int sqlite3_extensions_init(sqlite3* db, char** pzErrMsg, const sqlite3_api_routines* pApi)
{
    SQLITE_EXTENSION_INIT2(pApi);

    int result = SQLITE_OK;

    result |= sqlite3_create_function(db, "levenshtein", 2, SQLITE_UTF8, NULL, levenshtein_function, NULL, NULL);
    result |= sqlite3_create_function(db, "haversine", 4, SQLITE_UTF8, NULL, haversine_function, NULL, NULL);
    result |= sqlite3_create_function(db, "radToDist", 1, SQLITE_UTF8, NULL, radToDist_function, NULL, NULL);
    result |= sqlite3_create_function(db, "distToRad", 1, SQLITE_UTF8, NULL, distToRad_function, NULL, NULL);

    return result;
}