-- Create Cities table
CREATE TABLE Cities (
    city_id SERIAL PRIMARY KEY,
    city_name VARCHAR(255),
    city_routes INTEGER[]
);

-- Create Routes table
CREATE TABLE Routes (
    route_id SERIAL PRIMARY KEY,
    mileage DECIMAL,
    start_city_id INTEGER REFERENCES Cities(city_id) ON DELETE CASCADE,
    end_city_id INTEGER REFERENCES Cities(city_id) ON DELETE CASCADE
);

-- Create CityRoutes table
CREATE TABLE CityRoutes (
    city_id INTEGER REFERENCES Cities(city_id) ON DELETE CASCADE,
    route_id INTEGER REFERENCES Routes(route_id) ON DELETE CASCADE,
    PRIMARY KEY (city_id, route_id)
);

-- Create foreign key constraints for Routes table
ALTER TABLE Routes ADD CONSTRAINT FK_routes_start_city_id
    FOREIGN KEY (start_city_id) REFERENCES Cities(city_id) ON DELETE CASCADE;

ALTER TABLE Routes ADD CONSTRAINT FK_Routes_end_city_id
    FOREIGN KEY (end_city_id) REFERENCES Cities(city_id) ON DELETE CASCADE;
