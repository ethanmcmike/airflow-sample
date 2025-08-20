SELECT icao
FROM airlines
WHERE id = $id
LIMIT 1;