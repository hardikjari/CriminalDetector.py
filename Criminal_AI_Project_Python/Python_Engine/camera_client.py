# ==============================================================================
# camera_client.py (Individual Recognition Version) - FINAL
#
# This script watches the camera, identifies specific individuals from the
# watchlist, saves evidence, and reports it to the correct API endpoint.
# It now automatically detects the location.
# ==============================================================================

import os
import time
import json
from datetime import datetime, timezone
from typing import Optional, Dict

import cv2
import numpy as np
import requests
import geocoder

# --- CONFIGURATION SECTION ---

# -- API Settings --
# The base URL for your .NET API.
API_BASE_URL = "http://localhost:5263"
# The specific endpoint for reporting a detection event.
EVENT_API_PATH = "/api/Criminals/events"

# -- Camera and Detection Settings --
CAMERA_INDEX = 0
# --- ⬇️ THRESHOLD TUNING (THE FIX) ⬇️ ---
# With multiple people, conditions are worse. 55.0 is too strict.
# Let's try a more balanced value. Watch the scores on screen and adjust this
# value to be just above the score of your 'Unknowns' but low enough
# to catch the criminal. A good starting point is often 60-70.
LBPH_THRESHOLD = 68.0
# --- ⬆️ END FIX ⬆️ ---
COOLDOWN_SECONDS = 60 # Cooldown of 1 minute for the same person.
IMAGES_TO_CAPTURE = 5 # Number of images to save per detection event.

# -- Path Settings --
DETECTED_IMAGES_DIR = r"E:\HackAura\Criminal_AI_Project\Criminal_AI_Project_API\Criminal_AI_Project_API\wwwroot\images\detected"

# --- Internal Paths (Usually no need to change) ---
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
REPO_ROOT = os.path.abspath(os.path.join(SCRIPT_DIR, os.pardir))
MODEL_DIR = os.path.join(REPO_ROOT, "model_files")
CASCADE_PATH = os.path.join(SCRIPT_DIR, "haarcascade_frontalface_default.xml")
CLASSIFIER_PATH = os.path.join(MODEL_DIR, "classifier.xml")
LABELS_MAP_PATH = os.path.join(MODEL_DIR, "labels_map.json")
FACE_SIZE = (200, 200)

# --- Runtime Data (Do not change) ---
# In-memory store for the last detection time for each specific criminal GUID
last_detection_times: Dict[str, float] = {}

def load_cascade() -> cv2.CascadeClassifier:
    """Loads the Haar cascade classifier for face detection."""
    cascade = cv2.CascadeClassifier(CASCADE_PATH)
    if cascade.empty():
        raise RuntimeError(f"Failed to load Haar cascade at {CASCADE_PATH}")
    return cascade

def load_recognizer():
    """Loads the trained LBPH face recognizer model."""
    if not os.path.isfile(CLASSIFIER_PATH):
        return None
    recognizer = cv2.face.LBPHFaceRecognizer_create()
    recognizer.read(CLASSIFIER_PATH)
    return recognizer

def load_labels_map() -> Optional[Dict[str, str]]:
    """Loads the map that connects integer labels to string GUIDs."""
    if not os.path.isfile(LABELS_MAP_PATH):
        print("[ERROR] labels_map.json not found. Please train the model first.")
        return None
    with open(LABELS_MAP_PATH, "r", encoding="utf-8") as f:
        return json.load(f)

def get_model_mtime() -> Optional[float]:
    """Gets the last modification time of the model file."""
    return os.path.getmtime(CLASSIFIER_PATH) if os.path.isfile(CLASSIFIER_PATH) else None

def detect_faces(gray: np.ndarray, cascade: cv2.CascadeClassifier):
    """Detects faces in a grayscale image."""
    return cascade.detectMultiScale(gray, scaleFactor=1.2, minNeighbors=5, minSize=(50, 50))

def get_current_location() -> str:
    """
    Tries to find the current location using IP address.
    Returns a city and country, or a fallback string on failure.
    """
    try:
        g = geocoder.ip('me')
        if g.ok and g.city and g.country:
            return f"{g.city}, {g.country}"
        else:
            return "Unknown Location (Geocoder failed)"
    except Exception as e:
        print(f"[WARN] Could not get location. Check internet connection. Error: {e}")
        return "Location Service Offline"

def report_event(guid: str, event_time: datetime, location: str):
    """Sends a POST request to the events API with the correct payload."""
    url = f"{API_BASE_URL}{EVENT_API_PATH}"
    timestamp_str = event_time.isoformat().replace('+00:00', 'Z')
    payload = {"criminalGuid": guid, "eventAt": timestamp_str, "location": location}
    headers = {'Content-Type': 'application/json', 'accept': '*/*'}
    
    print(f"Reporting event to {url} with payload: {json.dumps(payload)}")
    try:
        response = requests.post(url, json=payload, headers=headers, timeout=10)
        if 200 <= response.status_code < 300:
            print(f"Successfully reported event for GUID: {guid}. Status: {response.status_code}")
        else:
            print(f"Failed to report event. Status: {response.status_code}, Response: {response.text}")
    except requests.exceptions.RequestException as e:
        print(f"An error occurred while calling the event API: {e}")

def main():
    """Main function to run the camera client."""
    current_location = get_current_location()

    print("--- Camera Client (Individual Recognition) ---")
    print(f"Watching camera index: {CAMERA_INDEX}")
    print(f"Reporting events to: {API_BASE_URL}{EVENT_API_PATH}")
    print(f"Location automatically detected as: {current_location}")
    print(f"Current confidence threshold: {LBPH_THRESHOLD} (Lower is stricter)")


    cascade = load_cascade()
    recognizer = load_recognizer()
    labels_map = load_labels_map()
    last_model_mtime = get_model_mtime()

    cap = cv2.VideoCapture(CAMERA_INDEX)
    if not cap.isOpened():
        raise RuntimeError(f"Failed to open camera index {CAMERA_INDEX}")

    try:
        while True:
            model_mtime = get_model_mtime()
            if model_mtime and model_mtime != last_model_mtime:
                print("[INFO] Detected model update. Reloading...")
                recognizer = load_recognizer()
                labels_map = load_labels_map()
                last_model_mtime = model_mtime

            ret, frame = cap.read()
            if not ret:
                time.sleep(0.1)
                continue

            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            detected_faces = detect_faces(gray, cascade)

            for (x, y, w, h) in detected_faces:
                if recognizer is None or labels_map is None:
                    cv2.rectangle(frame, (x, y), (x + w, y + h), (255, 0, 0), 2)
                    continue

                face_roi = cv2.resize(gray[y:y + h, x:x + w], FACE_SIZE)
                label, distance = recognizer.predict(face_roi)
                
                if label != -1 and distance <= LBPH_THRESHOLD:
                    guid = labels_map.get(str(label), "Error")
                    text = f"MATCH: {guid[:8]} ({distance:.2f})"
                    cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 255, 0), 2)
                    cv2.putText(frame, text, (x, y - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)

                    current_time = time.time()
                    last_seen_time = last_detection_times.get(guid)

                    if last_seen_time is None or (current_time - last_seen_time) > COOLDOWN_SECONDS:
                        print(f"Detected known individual with GUID: {guid}. Confidence: {distance:.2f}. Capturing evidence...")
                        event_time = datetime.now(timezone.utc)
                        timestamp_folder_name = event_time.strftime('%Y-%m-%d_%H-%M-%S')
                        save_directory = os.path.join(DETECTED_IMAGES_DIR, guid, timestamp_folder_name)
                        os.makedirs(save_directory, exist_ok=True)
                        
                        for i in range(IMAGES_TO_CAPTURE):
                            ret_burst, frame_burst = cap.read()
                            if not ret_burst: continue
                            img_name = f"capture_{i+1}_{event_time.strftime('%H%M%S%f')}.jpg"
                            full_path = os.path.join(save_directory, img_name)
                            cv2.imwrite(full_path, frame_burst)
                            print(f"  Saved image {i+1}/{IMAGES_TO_CAPTURE}: {img_name}")
                            time.sleep(0.2)
                        
                        report_event(guid, event_time, current_location)
                        last_detection_times[guid] = current_time
                else:
                    # --- ⬇️ IMPORTANT DEBUGGING IMPROVEMENT ⬇️ ---
                    # Now we show the distance for unknown faces. This helps you tune the threshold.
                    text = f"Unknown (dist: {distance:.2f})"
                    # --- ⬆️ END IMPROVEMENT ⬆️ ---
                    cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 0, 255), 2)
                    cv2.putText(frame, text, (x, y - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 0, 255), 2)

            cv2.imshow("Live Surveillance", frame)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

    except KeyboardInterrupt:
        print("\nExiting Camera Client...")
    finally:
        cap.release()
        cv2.destroyAllWindows()

if __name__ == "__main__":
    main()