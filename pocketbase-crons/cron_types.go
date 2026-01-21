package main

type CronRequest struct {
    ID         string         `json:"id"`
    Expression string         `json:"expression"`
    Payload    map[string]any `json:"payload"`
}
