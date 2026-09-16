# -*- coding: utf-8 -*-
"""95차: Tools/KlingGen 을 http://127.0.0.1:8765/ 로 서빙(CORS 허용) — 내장 브라우저(kling.ai 탭)가 plan95.json·ff_MOM 앵커를 fetch 하기 위해.
   start /b python Tools/KlingGen/kg_server.py  (run.bat 에서)  · 끝나면 창 닫거나 taskkill."""
import http.server, socketserver, os, sys
ROOT = os.path.dirname(os.path.abspath(__file__))
os.chdir(ROOT)
class H(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Cache-Control', 'no-store')
        super().end_headers()
    def log_message(self, *a): pass
socketserver.TCPServer.allow_reuse_address = True
with socketserver.TCPServer(('127.0.0.1', 8765), H) as httpd:
    httpd.serve_forever()
