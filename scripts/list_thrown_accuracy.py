"""List the accuracy of all thrown items starting with crpg_ from data/items.json."""

import json
import os

THROWN_CLASSES = {"Javelin", "ThrowingAxe", "ThrowingKnife", "Stone", "Boulder"}

def main():
    items_path = os.path.join(os.path.dirname(__file__), "..", "data", "items.json")
    with open(items_path, "r", encoding="utf-8") as f:
        items = json.load(f)

    thrown_items = []
    for item in items:
        if item.get("type") != "Thrown" or not item.get("id", "").startswith("crpg_"):
            continue
        for weapon in item.get("weapons", []):
            if weapon.get("class") in THROWN_CLASSES:
                thrown_items.append({
                    "id": item["id"],
                    "name": item.get("name", ""),
                    "class": weapon["class"],
                    "accuracy": weapon.get("accuracy", 0),
                })
                break

    thrown_items.sort(key=lambda x: x["accuracy"], reverse=True)

    print(f"{'Name':<40} {'Class':<15} {'Accuracy':>8}")
    print("-" * 65)
    for t in thrown_items:
        print(f"{t['name']:<40} {t['class']:<15} {t['accuracy']:>8}")
    avg = sum(t["accuracy"] for t in thrown_items) / len(thrown_items) if thrown_items else 0
    print(f"\nTotal: {len(thrown_items)} thrown items")
    print(f"Average accuracy: {avg:.2f}")


if __name__ == "__main__":
    main()
