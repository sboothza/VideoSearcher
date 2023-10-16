import json
import threading

import requests


class Translation:
    def __init__(self, source_text: str, detected_language: str, translated_text: str, translated_language: str):
        self.source_text = source_text
        self.detected_language = detected_language
        self.translated_text = translated_text
        self.translated_language = translated_language


def is_english(text: str, bypass: bool = False):
    if bypass:
        return False

    try:
        text.encode(encoding='utf-8').decode('ascii')
    except UnicodeDecodeError:
        return False
    else:
        return True


translated_already = {}

translations_lock = threading.Lock()


def save_translations(filename: str):
    if filename is None:
        return
    translations_lock.acquire()
    with (open(filename, "w") as f):
        f.write(json.dumps(translated_already))
    translations_lock.release()


def load_translations(filename: str):
    if filename is None:
        return
    try:
        translations_lock.acquire()
        with (open(filename, "r") as f):
            t = f.read()
            global translated_already
            translated_already = json.loads(t)
    except Exception as e:
        print(e)
    finally:
        translations_lock.release()


def translate_string(value: str, bypass: bool = False) -> str:
    if not is_english(value, bypass):
        print(f"translating {value}")
        new_value = translate_text(value)
        if new_value.translated_language == "en" and new_value.translated_text != new_value.source_text:
            print(f"successfully translated {value} to {new_value.translated_text}")
            return new_value.translated_text
        else:
            print(f"couldn't translate {value}")
    else:
        print(f"{value} is already in english")
    return value


def translate_text(text: str, language: str = "en") -> Translation:
    if text in translated_already:
        translated = translated_already[text]
        return Translation(text, "en", translated, "en")
    else:
        # t = translate_raw(text, language)
        t = translate_google(text)
        if t is not None:
            translated_already[t.source_text] = t.translated_text

        return t


def translate_raw(text: str, language: str = "en") -> Translation | None:
    payload = {
        "q": text,
        "source": "auto",
        "target": language,
        "format": "text",
        "api_key": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
    }
    # headers={"Content-Type": "application/json"}
    r = requests.post("https://libretranslate.de/translate", data=payload)
    if r.status_code == 200:
        data = r.json()
        result = Translation(text, data["detectedLanguage"]["language"], data["translatedText"], language)
        return result

    return None


def translate_google(text: str) -> Translation | None:
    url = "https://translate.googleapis.com/translate_a/single?client=gtx&sl={sl}&tl=en&dt=t&q={text}"
    r = requests.get(url.format(sl="auto", text=text))
    if r.status_code == 200:
        data = r.json()
        translated = data[0][0][0]
        source_language = data[2]
        result = Translation(text, source_language, translated, "en")
        return result

    return None
