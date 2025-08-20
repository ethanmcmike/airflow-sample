select airline, origin, destination
from operators as ops
WHERE ops.origin = $airport_id
OR ops.destination = $airport_id;