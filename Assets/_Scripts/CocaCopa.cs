using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CocaCopa.Utilities {
    public static class Common {
        /// <summary>
        /// Start a timer with the specified values
        /// </summary>
        /// <param name="waitTimer">Current timer value.</param>
        /// <param name="waitTime">Maximum timer value.</param>
        /// <param name="autoReset">True,  if you prefer the timer to reset automatically.</param>
        /// <returns>True, when the timer value is 0</returns>
        public static bool TickTimer(ref float waitTimer, float waitTime, bool autoReset = true) {
            if (waitTimer == 0) {
                if (autoReset)
                    waitTimer = waitTime;
                return true;
            }
            else {
                waitTimer -= Time.deltaTime;
                waitTimer = Mathf.Clamp(waitTimer, 0, waitTime);
            }
            return false;
        }

        /// <summary>
        /// Interpolate an animation curve based on distance and speed.
        /// </summary>
        /// <param name="curve">The animation curve to interpolate.</param>
        /// <param name="animationPoints">The progression along the curve.</param>
        /// <param name="speed">The speed of interpolation in meters per second (m/s).</param>
        /// <param name="distance">The total distance covered by the animation.</param>
        /// <param name="increment">Set to 'false' to interpolate in reverse.</param>
        /// <returns>The value on the curve corresponding to the progression.
        /// </returns>
        public static float EvaluateAnimationCurve(AnimationCurve curve, ref float animationPoints, float speed, float distance, bool increment = true) {
            if (increment)
                animationPoints += (speed / distance) * Time.deltaTime;
            else
                animationPoints -= (speed / distance) * Time.deltaTime;
            animationPoints = float.IsNaN(animationPoints) ? 0 : Mathf.Clamp01(animationPoints);
            return curve.Evaluate(animationPoints);
        }

        /// <summary>
        /// Interpolate an animation curve based on speed.
        /// </summary>
        /// <param name="curve">The animation curve to interpolate.</param>
        /// <param name="animationPoints">The progression along the curve.</param>
        /// <param name="speed">The speed of interpolation in meters per second (m/s).</param>
        /// <param name="increment">Set to 'false' to interpolate in reverse.</param>
        /// <returns>The value on the curve corresponding to the progression.
        /// </returns>
        public static float EvaluateAnimationCurve(AnimationCurve curve, ref float animationPoints, float speed, bool increment = true) {
            if (increment)
                animationPoints += speed * Time.deltaTime;
            else
                animationPoints -= speed * Time.deltaTime;
            animationPoints = float.IsNaN(animationPoints) ? 0 : Mathf.Clamp01(animationPoints);
            return curve.Evaluate(animationPoints);
        }

        /// <summary>
        /// Swaps the values between 2 given Vectors
        /// </summary>
        /// <param name="vector_1">Value 1.</param>
        /// <param name="vector_2">Value 2.</param>
        public static void SwapVectorValues(ref Vector3 vector_1, ref Vector3 vector_2) {
            Vector3 temp = vector_1;
            vector_1 = vector_2;
            vector_2 = temp;
        }

        /// <summary>
        /// Swaps the values between the 2 given floats
        /// </summary>
        /// <param name="float_1">Value 1.</param>
        /// <param name="float_2">Value 2.</param>
        public static void SwapFloatValues(ref float float_1, ref float float_2) {
            float temp = float_1;
            float_1 = float_2;
            float_2 = temp;
        }
    }

    public static class Environment {

        /// <summary>
        /// Generates a random point on a circle in a plane perpendicular to a given forward direction.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="forwardDirection">The direction perpendicular to the plane in which the circle lies.</param>
        /// <returns>A random point on the circle.</returns>
        public static Vector3 RandomVectorPointOnCircle(Vector3 center, float radius, Vector3 forwardDirection) {
            Vector3 perpendicular = Vector3.Cross(forwardDirection.normalized, Vector3.up).normalized;
            if (perpendicular == Vector3.zero) {
                perpendicular = Vector3.Cross(forwardDirection.normalized, Vector3.right).normalized;
            }
            Vector2 randomPoint2D = Random.insideUnitCircle * radius;
            Vector3 randomPoint = new Vector3(randomPoint2D.x, randomPoint2D.y);
            randomPoint = Quaternion.LookRotation(forwardDirection) * randomPoint;
            return center + randomPoint;
        }


        /// <summary>
        /// Checks if the given Vector3 is inside of the given box.
        /// </summary>
        /// <param name="transform">Transfom of the object calling the function.</param>
        /// <param name="point">Position to check.</param>
        /// <param name="limitsX">Box size X.</param>
        /// <param name="limitsZ">Box size Z.</param>
        /// <returns>True, if the given point is out of the specified limits.</returns>
        public static bool OutOfBounds(Transform transform, Vector3 point, float limitsX, float limitsZ) {
            Vector3 pointWorldToLocal = transform.InverseTransformPoint(point);
            bool outOfBoundsUp    = pointWorldToLocal.z - limitsZ / 2 > 0;
            bool outOfBoundsDown  = pointWorldToLocal.z + limitsZ / 2 < 0;
            bool outOfBoundsRight = pointWorldToLocal.x - limitsX / 2 > 0;
            bool outOfBoundsLeft  = pointWorldToLocal.x + limitsX / 2 < 0;
            return outOfBoundsUp || outOfBoundsDown || outOfBoundsRight || outOfBoundsLeft;
        }

        /// <summary>
        /// Finds the closest position to a target in a list of positions.
        /// </summary>
        /// <param name="target">The position of the target.</param>
        /// <param name="transforms">A list of transforms to compare against.</param>
        /// <returns>The closest position to the provided target position.</returns>
        public static Vector3 FindClosestPosition(Vector3 target, List<Transform> transforms) {
            List<Vector3> positions = new List<Vector3>();
            foreach (var transform in transforms) {
                positions.Add(transform.position);
            }
            return FindClosestPosition(target, positions);
        }

        /// <summary>
        /// Finds the closest position to a target in a list of positions.
        /// </summary>
        /// <param name="target">The position of the target.</param>
        /// <param name="positions">A list of positions to compare against.</param>
        /// <returns>The closest position to the provided target position.</returns>
        public static Vector3 FindClosestPosition(Vector3 target, List<Vector3> positions) {
            if (positions == null || positions.Count == 0) {
                Debug.LogWarning("List of positions is null or empty.");
                return Vector3.zero;
            }

            Vector3 closestPosition = positions[0];
            float closestDistanceSqr = (target - closestPosition).sqrMagnitude;

            for (int i = 1; i < positions.Count; i++) {
                float distanceSqr = (target - positions[i]).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr) {
                    closestDistanceSqr = distanceSqr;
                    closestPosition = positions[i];
                }
            }

            return closestPosition;
        }

        /// <summary>
        /// Checks if the given position is close to any of the positions in a list.
        /// </summary>
        /// <param name="position">Position to check.</param>
        /// <param name="positions">List of positions.</param>
        /// <param name="distance">Distance.</param>
        /// <returns>True, if the distance is less than the specified 'distance'.</returns>
        public static bool IsPositionCloseToAnyPosition(Vector3 position, List<Vector3> positions, float distance) {
            bool tooClose = false;
            for (int i = 0; i < positions.Count; i++) {
                if (Vector3.Distance(position, positions[i]) <= distance) {
                    tooClose = true;
                    break;
                }
            }
            return tooClose;
        }

        /// <summary>
        /// Checks if the given position is close to any of the positions in a list
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <param name="positions">List of positions</param>
        /// <param name="distances">List of distances</param>
        /// <returns>True, if the distance is less than the specified 'distances'</returns>
        public static bool IsPositionCloseToAnyPosition(Vector3 position, List<Vector3> positions, List<float> distances) {
            bool tooClose = false;
            for (int i = 0; i < positions.Count; i++) {
                if (Vector3.Distance(position, positions[i]) <= distances[i]) {
                    tooClose = true;
                    break;
                }
            }
            return tooClose;
        }

        /// <summary>
        /// Retrieves the world space positions of the four corners of the mesh rendered by the specified object's transform.
        /// </summary>
        /// <param name="objectTransform">The transform of the object whose mesh corners are to be calculated.</param>
        /// <returns>A list containing the world space positions of the four corners of the provided mesh.</returns>
        public static List<Vector3> GetMeshColliderEdges(Transform objectTransform, bool debugCalculatedEdges = false) {
            Mesh mesh;
            if (objectTransform.TryGetComponent<MeshFilter>(out var meshFilter)) {
                mesh = meshFilter.mesh;
            }
            else {
                mesh = objectTransform.GetComponentInChildren<MeshFilter>().mesh;
            }

            if (mesh == null) {
                Debug.LogError("Could not get a 'MeshFilter' component from the provided object.");
                return new List<Vector3>();
            }

            // Get the bounds of the mesh
            Bounds bounds = mesh.bounds;

            // Calculate the corners of the bounding box
            Vector3 frontBottomLeft = objectTransform.TransformPoint(bounds.min);
            Vector3 frontBottomRight = objectTransform.TransformPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));
            Vector3 backBottomLeft = objectTransform.TransformPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));
            Vector3 backBottomRight = objectTransform.TransformPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));

            if (debugCalculatedEdges) {
                Debug.DrawRay(frontBottomLeft, Vector3.up * 5f, Color.red, float.MaxValue);
                Debug.DrawRay(frontBottomRight, Vector3.up * 5f, Color.red, float.MaxValue);
                Debug.DrawRay(backBottomLeft, Vector3.up * 5f, Color.red, float.MaxValue);
                Debug.DrawRay(backBottomRight, Vector3.up * 5f, Color.red, float.MaxValue);
            }

            return new List<Vector3> {
                frontBottomLeft,
                frontBottomRight,
                backBottomLeft,
                backBottomRight
            };
        }

        /// <summary>
        /// Retrieves the world space positions of the four *bottom* corners of the provided box collider.
        /// </summary>
        /// <param name="collider">The box collider whose corners are to be calculated</param>
        /// <returns>A list containing the world space positions of the four corners of the provided box collider.</returns>
        public static List<Vector3> GetBoxColliderEdges(BoxCollider collider, bool debugCalculatedEdges = false) {
            Vector3 extents = collider.size / 2f;
            Vector3 forwardDirection = collider.transform.forward;
            Vector3 rightDirection = collider.transform.right;
            Vector3 upDirection = collider.transform.up;

            Vector3 frontLeftCorner = collider.bounds.center;
            frontLeftCorner += forwardDirection * extents.z;
            frontLeftCorner -= rightDirection * extents.x;
            frontLeftCorner -= upDirection * extents.y;

            Vector3 frontRightCorner = collider.bounds.center;
            frontRightCorner += forwardDirection * extents.z;
            frontRightCorner += rightDirection * extents.x;
            frontRightCorner -= upDirection * extents.y;

            Vector3 backLeftCorner = collider.bounds.center;
            backLeftCorner -= forwardDirection * extents.z;
            backLeftCorner -= rightDirection * extents.x;
            backLeftCorner -= upDirection * extents.y;

            Vector3 backRightCorner = collider.bounds.center;
            backRightCorner -= forwardDirection * extents.z;
            backRightCorner += rightDirection * extents.x;
            backRightCorner -= upDirection * extents.y;

            if (debugCalculatedEdges) {
                Debug.DrawRay(frontLeftCorner, Vector3.up * 100f, Color.green);
                Debug.DrawRay(frontRightCorner, Vector3.up * 100f, Color.red);
                Debug.DrawRay(backLeftCorner, Vector3.up * 100f, Color.yellow);
                Debug.DrawRay(backRightCorner, Vector3.up * 100f, Color.magenta);
            }

            return new List<Vector3> {
                frontLeftCorner,
                frontRightCorner,
                backLeftCorner,
                backRightCorner
            };
        }
    }

    public static class AudioUtils {
        /// <summary>
        /// Plays a random sound picked from the given sound array.
        /// </summary>
        /// <param name="audioSource">Target audio source.</param>
        /// <param name="soundsArray">List to choose sound from.</param>
        /// <param name="setAudioClip">Set to 'true' if you want to set the selected clip as the audioSource's clip.</param>
        public static void PlayRandomSound(AudioSource audioSource, AudioClip[] soundsArray, bool setAudioClip = false) {
            if (soundsArray.Length > 0) {
                int randomIndex = Random.Range(0, soundsArray.Length);
                AudioClip audioClip = soundsArray[randomIndex];
                if (setAudioClip) {
                    audioSource.clip = audioClip;
                }
                audioSource.PlayOneShot(audioClip);
            }
        }

        /// <summary>
        /// Plays a random sound picked from the given sound array.
        /// </summary>
        /// <param name="soundsArray">List to choose sound from.</param>
        /// <param name="position">Target position to play the audio clip.</param>
        public static void PlayRandomSoundAtPoint(AudioClip[] soundsArray, Vector3 position) {
            int randomIndex = Random.Range(0, soundsArray.Length);
            AudioSource.PlayClipAtPoint(soundsArray[randomIndex], position);
        }
    }
}
