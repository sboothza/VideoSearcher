import mysql.connector
import psycopg2
from elasticsearch import Elasticsearch
from elasticsearch import helpers

es = Elasticsearch(
    hosts="https://milleniumfalcon:9200",
    basic_auth=("elastic", "ZbA0ee9pc8DW-RCbAjKA"),  # elastic:ZbA0ee9pc8DW-RCbAjKA
    verify_certs=False,
)
connection = psycopg2.connect(
    user="postgres", password="or9asm1c", host="milleniumfalcon", database="video"
)

# connection = mysql.connector.connect(
#     user="root", password="or9asm1c", host="localhost", database="video"
# )

cursor = connection.cursor()  # (buffered=True)
cursor.execute(
    "select id, url, video_id, title, uploader, width, height, duration, "
    "filename, size, insertdate, downloaddate, description, tags from video_catalog where "
    "complete = 1 and deleted = 0 and filename <> ''"
)

actions = []
save_size = 100

for rec in cursor.fetchall():
    doc = {
        "url": rec[1],
        "video_id": rec[2],
        "title": rec[3],
        "uploader": rec[4],
        "width": rec[5],
        "height": rec[6],
        "duration": rec[7],
        "filename": rec[8],
        "size": rec[9],
        "insertdate": rec[10],
        "downloaddate": rec[11],
        "description": rec[12],
        "tags": rec[13],
    }

    action = {"_index": "video", "_op_type": "index", "_id": rec[0], "_source": doc}

    actions.append(action)
    if len(actions) >= save_size:
        helpers.bulk(es, actions)
        del actions[0 : len(actions)]

    print(rec[0])

helpers.bulk(es, actions)

connection.close()

es.indices.refresh(index="video")

resp = es.search(index="video", query={"match_all": {}})
print("Got %d Hits:" % resp["hits"]["total"]["value"])
# for hit in resp['hits']['hits']:
#     print("%(timestamp)s %(author)s: %(text)s" % hit["_source"])
