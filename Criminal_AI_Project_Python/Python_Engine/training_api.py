# ==============================================================================
# training_api.py - The AI Trainer ("The Brain Maker") - FINAL
#
# This script creates a small web server that waits for a command. When it
# receives a command to the '/train' URL, it looks at all the criminal photos
# and creates a new AI "brain" that can recognize EACH individual.
# ==============================================================================

import os
import json
import time
from datetime import datetime, timezone
from typing import Dict, List, Tuple

import cv2
import numpy as np
from flask import Flask, jsonify, request
from flask_cors import CORS
import requests

# --- CONFIGURATION SECTION ---
HOST = "0.0.0.0"
PORT = 5001
NOTIFICATION_API_BASE_URL = "http://localhost:5263"
# Ensure this path is correct
IMAGES_DIR = r"E:\HackAura\Criminal_AI_Project\Criminal_AI_Project_API\Criminal_AI_Project_API\wwwroot\images\criminals"

# --- Internal Paths (Usually no need to change) ---
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
REPO_ROOT = os.path.abspath(os.path.join(SCRIPT_DIR, os.pardir))
MODEL_DIR = os.path.join(REPO_ROOT, "model_files")
CASCADE_PATH = os.path.join(SCRIPT_DIR, "haarcascade_frontalface_default.xml")
CLASSIFIER_PATH = os.path.join(MODEL_DIR, "classifier.xml")
LABELS_MAP_PATH = os.path.join(MODEL_DIR, "labels_map.json")
META_PATH = os.path.join(MODEL_DIR, "metadata.json")
FACE_SIZE = (200, 200)


def ensure_dir(path: str) -> None:
    if not os.path.isdir(path):
        os.makedirs(path, exist_ok=True)

def load_cascade() -> cv2.CascadeClassifier:
    if not os.path.isfile(CASCADE_PATH):
        raise RuntimeError(f"Haar Cascade file not found at {CASCADE_PATH}")
    cascade = cv2.CascadeClassifier(CASCADE_PATH)
    if cascade.empty():
        raise RuntimeError(f"Failed to load Haar cascade at {CASCADE_PATH}")
    return cascade

def iter_image_files(images_dir: str) -> List[str]:
    if not os.path.isdir(images_dir):
        print(f"[WARN] Images directory not found at: {images_dir}")
        return []
    exts = {".jpg", ".jpeg", ".png", ".bmp"}
    files = []
    for name in os.listdir(images_dir):
        path = os.path.join(images_dir, name)
        if os.path.isfile(path) and os.path.splitext(name)[1].lower() in exts:
            files.append(path)
    files.sort()
    return files

def detect_largest_face(gray_image: np.ndarray, cascade: cv2.CascadeClassifier) -> np.ndarray:
    faces = cascade.detectMultiScale(gray_image, scaleFactor=1.2, minNeighbors=5, minSize=(50, 50))
    if len(faces) == 0:
        return None
    x, y, w, h = max(faces, key=lambda f: f[2] * f[3])
    face = gray_image[y:y + h, x:x + w]
    face = cv2.resize(face, FACE_SIZE)
    return face

def build_dataset(images_dir: str) -> Tuple[List[np.ndarray], List[int], Dict[int, str], int]:
    """Processes all images to create the training data. Each image is a unique person."""
    cascade = load_cascade()
    faces: List[np.ndarray] = []
    labels: List[int] = []
    id_to_guid_map: Dict[int, str] = {}
    current_id = 0
    total_images_with_faces = 0

    files = iter_image_files(images_dir)

    for img_path in files:
        img = cv2.imread(img_path)
        if img is None: continue
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        face = detect_largest_face(gray, cascade)
        if face is None:
            print(f"[WARN] No face found in {os.path.basename(img_path)}. Skipping.")
            continue

        # The filename (without extension) IS the GUID
        guid = os.path.splitext(os.path.basename(img_path))[0]

        faces.append(face)
        labels.append(current_id)
        id_to_guid_map[current_id] = guid
        current_id += 1 # Increment ID for the next unique person
        total_images_with_faces += 1

    return faces, labels, id_to_guid_map, total_images_with_faces

def train_lbph(faces: List[np.ndarray], labels: List[int]):
    if len(faces) == 0:
        raise ValueError("No faces found to train on")
    recognizer = cv2.face.LBPHFaceRecognizer_create()
    recognizer.train(faces, np.array(labels))
    return recognizer

def save_model(recognizer, id_to_guid_map: Dict[int, str]) -> Dict[str, str]:
    ensure_dir(MODEL_DIR)
    recognizer.write(CLASSIFIER_PATH)
    with open(LABELS_MAP_PATH, "w", encoding="utf-8") as f:
        json.dump(id_to_guid_map, f, indent=2)
    meta = { "trained_at": datetime.now(timezone.utc).isoformat(), "total_individuals": len(id_to_guid_map) }
    with open(META_PATH, "w", encoding="utf-8") as f:
        json.dump(meta, f, indent=2)
    return meta

def notify_training_complete(num_images: int):
    url = f"{NOTIFICATION_API_BASE_URL}/api/Criminals/training"
    timestamp = datetime.now(timezone.utc).isoformat().replace('+00:00', 'Z')
    payload = { "trainedAt": timestamp, "numberOfImagesTrained": num_images }
    headers = { 'Content-Type': 'application/json', 'accept': '*/*' }
    print(f"Notifying API at {url} with payload: {json.dumps(payload)}")
    try:
        response = requests.post(url, json=payload, headers=headers, timeout=10)
        if 200 <= response.status_code < 300:
            print(f"Successfully notified API. Status: {response.status_code}")
        else:
            print(f"Failed to notify API. Status: {response.status_code}, Response: {response.text}")
    except requests.exceptions.RequestException as e:
        print(f"An error occurred while calling the notification API: {e}")

# --- WEB SERVER (API) SECTION ---
app = Flask(__name__)
CORS(app)

@app.post("/train")
def train_endpoint():
    print("Received /train request. Starting individual recognition training...")
    start = time.time()
    try:
        faces, labels, id_to_guid_map, total_images = build_dataset(IMAGES_DIR)
        if len(faces) == 0:
            return jsonify({"success": False, "message": "No usable faces found in images directory."}), 400
        recognizer = train_lbph(faces, labels)
        save_model(recognizer, id_to_guid_map)
        elapsed = time.time() - start
        print(f"Training completed successfully for {len(id_to_guid_map)} individuals in {elapsed:.2f} seconds.")
        notify_training_complete(total_images)
        return jsonify({ "success": True, "message": "Training completed" })
    except Exception as e:
        print(f"[ERROR] Training failed: {e}")
        return jsonify({"success": False, "message": str(e)}), 500

def main():
    print(f"--- AI Trainer API (Individual Recognition) ---")
    print(f"Starting on http://{HOST}:{PORT}")
    print(f"Reading images from: {IMAGES_DIR}")
    print(f"Saving model to: {MODEL_DIR}")
    print(f"Will notify training completion to: {NOTIFICATION_API_BASE_URL}")
    app.run(host=HOST, port=PORT, debug=False)

if __name__ == "__main__":
    main()