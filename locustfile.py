from locust import HttpUser, task, between
import random

class TrilloUser(HttpUser):
    wait_time = between(1, 3)
    
    board_ids = []
    card_ids = []

    # BOARDS

    @task(2)
    def get_all_boards(self):
        self.client.get("/boards")

    @task(1)
    def create_board(self):
        with self.client.post("/boards", json={
            "name": "Locust Board",
            "description": "Created during load test"
        }, catch_response=True) as response:
            if response.status_code == 201:
                board = response.json()
                TrilloUser.board_ids.append(board["id"])
            else:
                response.failure("Failed to create board")

    @task(1)
    def update_board(self):
        if TrilloUser.board_ids:
            board_id = random.choice(TrilloUser.board_ids)
            self.client.put(f"/boards/{board_id}", json={
                "name": "Updated Board",
                "description": "Updated by locust"
            })

    @task(1)
    def delete_board(self):
        if TrilloUser.board_ids:
            board_id = TrilloUser.board_ids.pop(0) 
            self.client.delete(f"/boards/{board_id}")

    # CARDS

    @task(2)
    def get_all_cards(self):
        self.client.get("/cards")

    @task(1)
    def create_card(self):
        if TrilloUser.board_ids:  
            board_id = random.choice(TrilloUser.board_ids)
            with self.client.post("/cards", json={
                "title": "Locust Card",
                "description": "Created during load test",
                "boardId": board_id
            }, catch_response=True) as response:
                if response.status_code == 201:
                    card = response.json()
                    TrilloUser.card_ids.append(card["id"])
                else:
                    response.failure("Failed to create card")

    @task(1)
    def update_card(self):
        if TrilloUser.card_ids:
            card_id = random.choice(TrilloUser.card_ids)
            self.client.put(f"/cards/{card_id}", json={
                "title": "Updated Card",
                "description": "Updated by locust"
            })

    @task(1)
    def delete_card(self):
        if TrilloUser.card_ids:
            card_id = TrilloUser.card_ids.pop(0)
            self.client.delete(f"/cards/{card_id}")
