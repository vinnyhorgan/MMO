bitser = require("lib/bitser")
Class = require("lib/class")
inspect = require("lib/inspect")
loader = require("lib/loader")
signal = require("lib/signal")
state = require("lib/gamestate")
sock = require("lib/sock")
timer = require("lib/timer")
vector = require("lib/vector")

-- Globals

le = love.event
lf = love.filesystem
lk = love.keyboard
lm = love.mouse
ls = love.system
lt = love.timer

function love.load(args)
    loader.load("src")

    state.registerEvents()

    state.switch(server)
end
