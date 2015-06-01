#include <iostream>
#include <fstream>

class HitArea
{
public:

	HitArea(const std::string& file);
	~HitArea();

	bool test(int x, int y);

private:

	HitArea();
	HitArea(const HitArea&);
	HitArea& operator= (const HitArea&);

protected:

	short _width;
	short _height;
	unsigned char* _area;
};


HitArea::HitArea(const std::string& file)
	: _width(0)
	, _height(0)
	, _area(NULL)
{
	std::ifstream fs(file.c_str(), std::ios::in | std::ios::binary);

	if (fs)
	{
		fs.read((char*)&_width, sizeof(_width));
		fs.read((char*)&_height, sizeof(_height));

		int size = _width / 8 * _height;
		_area = new unsigned char[size];

		fs.read((char*)_area, size);
	}
}

HitArea::~HitArea()
{
	if (_area)
		delete[] _area;
}

bool HitArea::test(int x, int y)
{
	if (x < 0 || x >= _width || y < 0 || y > _height)
		return false;

	int bytesPerLine = _width / 8;
	int hitByte = bytesPerLine * y + x / 8;
	int hitBit = x % 8;

	return !!((_area[hitByte] >> hitBit) & 1);
}

int main(void)
{
	HitArea hit("test.hit");

	return 0;
}
