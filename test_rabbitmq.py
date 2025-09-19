#!/usr/bin/env python3
import requests
from requests.auth import HTTPBasicAuth
import json

def test_rabbitmq_web():
    base_url = "http://localhost:15672"
    
    print("🔍 Testing RabbitMQ Web Interface...")
    
    # Test 1: Homepage
    try:
        response = requests.get(base_url, timeout=10)
        print(f"✅ Homepage: {response.status_code}")
        if "RabbitMQ Management" in response.text:
            print("✅ Page title found")
        else:
            print("❌ Page title missing")
    except Exception as e:
        print(f"❌ Homepage error: {e}")
    
    # Test 2: API with guest user
    try:
        api_url = f"{base_url}/api/overview"
        response = requests.get(api_url, auth=HTTPBasicAuth('guest', 'guest'), timeout=10)
        print(f"✅ API (guest): {response.status_code}")
        if response.status_code == 200:
            data = response.json()
            print(f"✅ RabbitMQ Version: {data.get('rabbitmq_version', 'Unknown')}")
        else:
            print(f"❌ API Error: {response.text}")
    except Exception as e:
        print(f"❌ API error: {e}")
    
    # Test 3: Static resources
    try:
        js_url = f"{base_url}/js/main.js"
        response = requests.get(js_url, timeout=10)
        print(f"✅ JavaScript: {response.status_code}")
    except Exception as e:
        print(f"❌ JavaScript error: {e}")
    
    try:
        css_url = f"{base_url}/css/main.css"
        response = requests.get(css_url, timeout=10)
        print(f"✅ CSS: {response.status_code}")
    except Exception as e:
        print(f"❌ CSS error: {e}")

if __name__ == "__main__":
    test_rabbitmq_web()