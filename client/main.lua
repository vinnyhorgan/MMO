bitser = require("lib/bitser")
Class = require("lib/class")
inspect = require("lib/inspect")
loader = require("lib/loader")
push = require("lib/push")
signal = require("lib/signal")
state = require("lib/gamestate")
sock = require("lib/sock")
timer = require("lib/timer")
vector = require("lib/vector")
require("lib/gooi")

-- Globals

la = love.audio
le = love.event
lf = love.filesystem
lg = love.graphics
lk = love.keyboard
lm = love.mouse
ls = love.system
lt = love.timer
lw = love.window

gameWidth = 800
gameHeight = 600

function love.load(args)
    loader.load("src")

    lg.setDefaultFilter("nearest", "nearest")

    state.registerEvents()


    if ls.getOS() == "Android" then
        local screenWidth, screenHeight = love.window.getDesktopDimensions()
        push:setupScreen(gameWidth, gameHeight, screenWidth, screenHeight, { fullscreen = true, resizable = false })
    else
        local screenWidth, screenHeight = love.window.getDesktopDimensions()
        push:setupScreen(gameWidth, gameHeight, gameWidth, gameHeight, { fullscreen = false, resizable = true })

        gooi.desktopMode()
    end

    state.switch(game)
end

function love.resize(w, h)
    return push:resize(w, h)
end
