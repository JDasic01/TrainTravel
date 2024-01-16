-- Create Cities table
CREATE TABLE Cities (
    CityId SERIAL PRIMARY KEY,
    CityName VARCHAR(255)
);

-- Create Routes table
CREATE TABLE Routes (
    RouteId SERIAL PRIMARY KEY,
    Mileage DECIMAL,
    StartCityId INTEGER REFERENCES Cities(CityId) ON DELETE CASCADE,
    EndCityId INTEGER REFERENCES Cities(CityId) ON DELETE CASCADE
);

-- Create CityRoutes table
CREATE TABLE CityRoutes (
    CityId INTEGER REFERENCES Cities(CityId) ON DELETE CASCADE,
    RouteId INTEGER REFERENCES Routes(RouteId) ON DELETE CASCADE,
    PRIMARY KEY (CityId, RouteId)
);

-- Create foreign key constraints for Routes table
ALTER TABLE Routes ADD CONSTRAINT FK_Routes_StartCityId
    FOREIGN KEY (StartCityId) REFERENCES Cities(CityId) ON DELETE CASCADE;

ALTER TABLE Routes ADD CONSTRAINT FK_Routes_EndCityId
    FOREIGN KEY (EndCityId) REFERENCES Cities(CityId) ON DELETE CASCADE;
